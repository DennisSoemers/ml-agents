"""
RQ1: Can we predict training time based on hyperparameters and hardware?
This script trains different ML models and compares their performance.
"""

import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split, cross_val_score
from sklearn.ensemble import RandomForestRegressor, GradientBoostingRegressor
from sklearn.metrics import mean_absolute_error, r2_score
import matplotlib.pyplot as plt
import yaml

# load configuration from yaml file
f = open('config.yaml')
config = yaml.safe_load(f)
f.close()
cfg = config['rq1']

# load the dataset
df = pd.read_csv(cfg['data_file'])
print("loaded", len(df), "samples")

# removing runs that took way too long or too short (bc they are probably failed runs)
df = df[df['total_time_seconds'] < cfg['preprocessing']['max_time']]
df = df[df['total_time_seconds'] > cfg['preprocessing']['min_time']]
print("after removing outliers:", len(df))

# convert game names each game gets its own column with 0 or 1
games = df['game_name'].unique()
for g in games:
    df['game_' + g] = 0
    df.loc[df['game_name'] == g, 'game_' + g] = 1

#creating new features that might help prediction
df['steps_per_batch'] = df['max_steps'] / df['batch_size']
df['buffer_ratio'] = df['buffer_size'] / df['batch_size']
df['network_size'] = df['hidden_units'] * df['num_layers']
df['lr_steps'] = df['learning_rate'] * df['max_steps']
df['batch_network'] = df['batch_size'] * df['network_size']
df['complexity'] = df['max_steps'] * df['num_layers'] * df['hidden_units']
# log transforming some features because they have large ranges
df['log_max_steps'] = np.log1p(df['max_steps'])
df['log_batch'] = np.log1p(df['batch_size'])
df['log_buffer'] = np.log1p(df['buffer_size'])
df['log_complexity'] = np.log1p(df['complexity'])
# target variable (use log transform because training times vary alot)
df['target'] = np.log1p(df['total_time_seconds'])
# build feature list from config file
feat = []
for f in cfg['features']['hardware']:
    feat.append(f)
for f in cfg['features']['hyperparameters']:
    feat.append(f)
# add our engineered features
feat.append('steps_per_batch')
feat.append('buffer_ratio')
feat.append('network_size')
feat.append('lr_steps')
feat.append('batch_network')
feat.append('log_max_steps')
feat.append('log_batch')
feat.append('log_buffer')
feat.append('log_complexity')
# add game columns
for c in df.columns:
    if c.startswith('game_'):
        feat.append(c)
X = df[feat]
y = df['target']
# filter to only numeric columns (some might be strings)
cols = []
for c in X.columns:
    if X[c].dtype in ['int64', 'float64']:
        cols.append(c)
X = X[cols]
# removing features with zero variance (bc they dont help prediction)
for c in X.columns:
    if X[c].var() == 0:
        X = X.drop(columns=[c])

# handle missing values
X = X.dropna()
y = y[X.index]
y_orig = df.loc[X.index, 'total_time_seconds']
print("using", len(X.columns), "features")
# splitting into training and test sets (80% train, 20% test)
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
# we also need the original times for evaluation (not log transformed)
idx_train, idx_test = train_test_split(X.index, test_size=0.2, random_state=42)
y_test_orig = df.loc[idx_test, 'total_time_seconds']
# convert back from log to normal seconds
def to_seconds(pred):
    return np.expm1(pred)
# random forest
rf_cfg = cfg['random_forest']
rf = RandomForestRegressor(
    n_estimators=rf_cfg['n_estimators'],
    max_depth=rf_cfg['max_depth'],
    min_samples_leaf=rf_cfg['min_samples_leaf'],
    random_state=cfg['random_state']
)
rf.fit(X_train, y_train)
rf_pred = to_seconds(rf.predict(X_test))
r2_rf = r2_score(y_test_orig, rf_pred)
mae_rf = mean_absolute_error(y_test_orig, rf_pred)
# gradient boosting
gb_cfg = cfg['gradient_boosting']
gb = GradientBoostingRegressor(
    n_estimators=gb_cfg['n_estimators'],
    max_depth=gb_cfg['max_depth'],
    learning_rate=gb_cfg['learning_rate'],
    random_state=cfg['random_state']
)
gb.fit(X_train, y_train)
gb_pred = to_seconds(gb.predict(X_test))
r2_gb = r2_score(y_test_orig, gb_pred)
mae_gb = mean_absolute_error(y_test_orig, gb_pred)
print("Random Forest:     R2=" + str(round(r2_rf, 3)) + " MAE=" + str(round(mae_rf, 0)) + "s")
print("Gradient Boosting: R2=" + str(round(r2_gb, 3)) + " MAE=" + str(round(mae_gb, 0)) + "s")
# checking which one is better
if r2_gb > r2_rf:
    best_name = "Gradient Boosting"
    best_r2 = r2_gb
    best_pred = gb_pred
else:
    best_name = "Random Forest"
    best_r2 = r2_rf
    best_pred = rf_pred

print("\nBest model: " + best_name + " with R2=" + str(round(best_r2, 3)))


# cross validation to check if results are consistent
print("\nCross validation (5-fold):")
cv_rf = cross_val_score(rf, X, y, cv=5, scoring='r2')
cv_gb = cross_val_score(gb, X, y, cv=5, scoring='r2')
print("Random Forest:     mean=" + str(round(cv_rf.mean(), 3)) + " std=" + str(round(cv_rf.std(), 3)))
print("Gradient Boosting: mean=" + str(round(cv_gb.mean(), 3)) + " std=" + str(round(cv_gb.std(), 3)))


# feature importance from random forest
print("\nTop features:")
importances = rf.feature_importances_
feat_names = X.columns.tolist()

# put them together and sort
feat_imp = []
for i in range(len(feat_names)):
    feat_imp.append([feat_names[i], importances[i]])
feat_imp.sort(key=lambda x: x[1], reverse=True)

for i in range(5):
    print(feat_imp[i][0] + ": " + str(round(feat_imp[i][1], 3)))
# stats per game
print("\nPer game stats:")
for g in games:
    subset = df[df['game_name'] == g]
    if len(subset) >= 10:
        avg = subset['total_time_seconds'].mean()
        print(g + ": n=" + str(len(subset)) + " avg=" + str(round(avg, 0)) + "s")


# training separate models for each game (predicting time directly, no log transform)
print("\nPer game models:")
per_game_results = []
for g in games:
    gdf = df[df['game_name'] == g]
    if len(gdf) >= cfg['per_game']['min_samples']:  # need at least 300 samples for reliable results
        # not using game columns for per-game model
        gcols = [c for c in X.columns if not c.startswith('game_')]
        Xg = gdf[gcols].dropna()
        yg_orig = gdf.loc[Xg.index, 'total_time_seconds']

        if len(Xg) >= cfg['per_game']['min_samples']:
            Xg_train, Xg_test, yg_train, yg_test = train_test_split(Xg, yg_orig, test_size=0.2, random_state=42)

            grf = RandomForestRegressor(n_estimators=200, max_depth=None, random_state=42)
            grf.fit(Xg_train, yg_train)
            gpred = grf.predict(Xg_test)
            gr2 = r2_score(yg_test, gpred)

            per_game_results.append([g, len(Xg), gr2])
            print(g + ": n=" + str(len(Xg)) + " R2=" + str(round(gr2, 3)))

per_game_results.sort(key=lambda x: x[2], reverse=True)


# ploting
fig1 = plt.figure(figsize=(10, 8))

# predicted vs actual
ax1 = fig1.add_subplot(2, 2, 1)
ax1.scatter(y_test_orig, best_pred, alpha=0.5, s=10)
minv = min(y_test_orig.min(), min(best_pred))
maxv = max(y_test_orig.max(), max(best_pred))
ax1.plot([minv, maxv], [minv, maxv], 'r--')
ax1.set_xlabel('Actual Time (s)')
ax1.set_ylabel('Predicted Time (s)')
ax1.set_title(best_name + ' (R2=' + str(round(best_r2, 3)) + ')')

# feature importance bar chart
ax2 = fig1.add_subplot(2, 2, 2)
top_feat = feat_imp[:8]
ax2.barh([x[0] for x in top_feat], [x[1] for x in top_feat])
ax2.set_xlabel('Importance')
ax2.set_title('Top Features')
ax2.invert_yaxis()

# avg time per game
ax3 = fig1.add_subplot(2, 2, 3)
game_avg = df.groupby('game_name')['total_time_seconds'].mean().sort_values()
ax3.barh(game_avg.index, game_avg.values)
ax3.set_xlabel('Average Time (s)')
ax3.set_title('Training Time by Game')

# eror histogram
ax4 = fig1.add_subplot(2, 2, 4)
errs = y_test_orig.values - best_pred
ax4.hist(errs, bins=30, edgecolor='black')
ax4.axvline(0, color='r', linestyle='--')
ax4.set_xlabel('Prediction Error (s)')
ax4.set_title('Error Distribution')

plt.tight_layout()
plt.savefig(cfg['output_plot'])
print("\nSaved plot to", cfg['output_plot'])


# second figure with more plots
fig2 = plt.figure(figsize=(10, 8))

# distribution of times
ax5 = fig2.add_subplot(2, 2, 1)
ax5.hist(df['total_time_seconds'], bins=50, edgecolor='black', alpha=0.7)
ax5.set_xlabel('Training Time (s)')
ax5.set_ylabel('Count')
ax5.set_title('Distribution of Training Times')
ax5.axvline(df['total_time_seconds'].mean(), color='r', linestyle='--', label='Mean')
ax5.legend()

# time vs steps scatter
ax6 = fig2.add_subplot(2, 2, 2)
ax6.scatter(df['max_steps'], df['total_time_seconds'], alpha=0.3, s=5)
ax6.set_xlabel('Max Steps')
ax6.set_ylabel('Training Time (s)')
ax6.set_title('Training Time vs Max Steps')

# samples per game
ax7 = fig2.add_subplot(2, 2, 3)
gcounts = df['game_name'].value_counts().sort_values()
ax7.barh(gcounts.index, gcounts.values, color='steelblue')
ax7.set_xlabel('Number of Samples')
ax7.set_title('Samples per Game')

# model comparison
ax8 = fig2.add_subplot(2, 2, 4)
ax8.bar(['Random Forest', 'Gradient Boosting'], [r2_rf, r2_gb], color=['steelblue', 'green'])
ax8.set_ylabel('R2 Score')
ax8.set_title('Model Comparison')
ax8.set_ylim(0, 1.0)

plt.tight_layout()
plt.savefig('time_prediction_analysis.png')
print("Saved plot to time_prediction_analysis.png")


# save results to file
f = open('results_summary.txt', 'w')
f.write("RQ1: Training Time Prediction Results\n")
f.write("======================================\n\n")
f.write("Dataset: " + str(len(X)) + " samples, " + str(len(X.columns)) + " features\n\n")
f.write("Model Performance (on test set):\n")
f.write("  Random Forest:     R2=" + str(round(r2_rf, 3)) + " MAE=" + str(round(mae_rf, 0)) + "s\n")
f.write("  Gradient Boosting: R2=" + str(round(r2_gb, 3)) + " MAE=" + str(round(mae_gb, 0)) + "s\n")
f.write("\nBest Model: " + best_name + " (R2=" + str(round(best_r2, 3)) + ")\n")
f.write("\nCross Validation (5-fold):\n")
f.write("  Random Forest:     mean=" + str(round(cv_rf.mean(), 3)) + " std=" + str(round(cv_rf.std(), 3)) + "\n")
f.write("  Gradient Boosting: mean=" + str(round(cv_gb.mean(), 3)) + " std=" + str(round(cv_gb.std(), 3)) + "\n")
print("(CV scores are on log scale)")

f.write("\nTop 5 Important Features:\n")
for i in range(5):
    f.write("  " + feat_imp[i][0] + ": " + str(round(feat_imp[i][1], 3)) + "\n")
f.write("\nPer Game Model Results:\n")
for r in per_game_results:
    f.write("  " + r[0] + ": n=" + str(r[1]) + " R2=" + str(round(r[2], 3)) + "\n")
f.close()
print("Saved results to results_summary.txt")
