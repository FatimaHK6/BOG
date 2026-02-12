import { test, expect } from '@playwright/test';

test('test', async ({ page }) => {
  await page.goto('http://localhost:4200/');
  await page.getByText('WelcomeRocket Shipbog-app app').click();
  await page.goto('http://localhost:4200/');
  await page.locator('#webpack-dev-server-client-overlay').contentFrame().getByText('Compiled with problems:XERROR').click();
  await page.locator('#webpack-dev-server-client-overlay').contentFrame().getByText('Compiled with problems:XERROR').click();
});