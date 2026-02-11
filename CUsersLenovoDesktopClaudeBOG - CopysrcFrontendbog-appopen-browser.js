const { chromium } = require('@playwright/test');

(async () => {
  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();
  
  console.log('Opening http://localhost:4200...');
  await page.goto('http://localhost:4200', { waitUntil: 'domcontentloaded', timeout: 10000 });
  
  console.log('Page loaded! Browser window is open.');
  console.log('The application should be visible in your browser.');
  
  // Keep browser open for 30 seconds
  await page.waitForTimeout(30000);
  
  await browser.close();
})();
