# Angular 13 → 18 Upgrade Baseline Documentation

**Date**: 2026-02-11
**Branch**: `feature/angular-18-upgrade`
**Backup Tag**: `pre-angular-18-upgrade`

## Current Versions

### Angular & Core Packages
- Angular CLI: 13.3.11
- Angular Core: 13.3.12
- TypeScript: 4.6.4 (4.6.2 in package.json)
- Node: 20.9.0 (marked as unsupported by Angular 13)
- npm: 10.1.0

### Key Dependencies
- @angular/animations: 13.3.12
- @angular/common: 13.3.12
- @angular/compiler: 13.3.12
- @angular/core: 13.3.12
- @angular/forms: 13.3.12
- @angular/platform-browser: 13.3.12
- @angular/platform-browser-dynamic: 13.3.12
- @angular/router: 13.3.12
- @angular/material: 13.3.9
- @angular/cdk: 13.3.9
- rxjs: 7.5.7 (7.5.0 in package.json)
- zone.js: 0.11.4
- ngx-editor: 12.2.1
- Cypress: 15.9.0

### Testing Framework
- Karma: 6.3.20
- Jasmine: 4.0.1
- karma-jasmine: 4.0.2
- karma-chrome-launcher: 3.1.1
- karma-coverage: 2.1.1

### Build Information
- Build tool: webpack (angular-devkit/build-angular 13.3.11)
- Expected builder change: webpack → esbuild (in Phase 5)

## Pre-Upgrade Checklist

- [x] Created feature branch: `feature/angular-18-upgrade`
- [x] Created backup tag: `pre-angular-18-upgrade`
- [x] Documented Angular version: 13.3.12
- [x] Documented TypeScript version: 4.6.4
- [ ] Run unit tests (baseline)
- [ ] Run E2E tests (baseline)
- [ ] Build production bundle (baseline)
- [ ] Verify API integration

## Known Issues/Notes

1. **Node version**: 20.9.0 is not officially supported by Angular 13, but runtime appears functional
2. **Material migration**: Will require 45 CSS class updates (.mat-* → .mat-mdc-*)
3. **ngx-editor**: Currently v12.2.1, needs upgrade to v17-18 for Angular 18
4. **Custom dev-server**: Project uses custom Express proxy (dev-server.js) - needs testing after esbuild migration

## Test Results (Baseline)

### Unit Tests
Status: [PENDING]

### E2E Tests (Cypress)
Status: [PENDING]

### Build Output
Status: [PENDING]

## Rollback Info

To rollback to pre-upgrade state:
```bash
git reset --hard pre-angular-18-upgrade
rm -rf node_modules package-lock.json .angular dist
npm install
npm start
```
