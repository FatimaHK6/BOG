const playwright = require('@playwright/test');

(async () => {
  const browser = await playwright.chromium.launch({ headless: true });
  const page = await browser.newPage();
  
  try {
    await page.goto('http://localhost:4200/case-registration/create', { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    
    // Click defendants tab
    const navItems = page.locator('nav li, [role="listitem"]');
    const allItems = await navItems.allTextContents();
    
    for (let i = 0; i < allItems.length; i++) {
      if (allItems[i].includes('المدعى عليهم')) {
        await navItems.nth(i).click();
        await page.waitForTimeout(1500);
        break;
      }
    }
    
    // Get all buttons in the defendants component
    const buttons = page.locator('app-defendant-list button');
    const buttonCount = await buttons.count();
    
    console.log(`Found ${buttonCount} buttons in defendants component:\n`);
    
    const buttonTexts = await buttons.allTextContents();
    buttonTexts.forEach((text, i) => {
      console.log(`${i + 1}. "${text.trim()}"`);
    });
    
    // Get all elements in the defendant container
    const container = page.locator('app-defendant-list');
    const html = await container.innerHTML();
    
    console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log('Component HTML structure:');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');
    
    // Log key elements
    if (html.includes('mat-menu')) {
      console.log('✓ mat-menu found in component');
    }
    if (html.includes('matMenuTriggerFor')) {
      console.log('✓ matMenuTriggerFor directive found');
    }
    if (html.includes('isEmpty')) {
      console.log('✓ isEmpty check (empty state logic)');
    }
    if (html.includes('mat-card')) {
      console.log('✓ mat-card found');
    }
    if (html.includes('defendant')) {
      console.log('✓ Defendant-related content present');
    }
    
    // Check for specific button content
    if (html.includes('إضافة')) {
      console.log('✓ "Add" button (إضافة) text found');
    }
    if (html.includes('حفظ')) {
      console.log('✓ "Save" button (حفظ) text found');
    }
    
  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    await browser.close();
  }
})();
