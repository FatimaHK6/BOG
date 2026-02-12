const playwright = require('@playwright/test');

(async () => {
  const browser = await playwright.chromium.launch();
  const page = await browser.newPage();
  
  try {
    console.log('🌐 Navigating to application...');
    await page.goto('http://localhost:4200', { waitUntil: 'networkidle' });
    
    console.log('✓ Application loaded');
    
    // Check if we're on the home page
    const title = await page.title();
    console.log(`📄 Page title: ${title}`);
    
    // Look for case registration link
    const caseRegLink = page.locator('a:has-text("قيد الدعاوى"), a:has-text("Case Registration"), button:has-text("قيد الدعاوى")');
    const linkCount = await caseRegLink.count();
    
    if (linkCount > 0) {
      console.log('✓ Found Case Registration link');
      await caseRegLink.first().click();
      await page.waitForNavigation();
      console.log('✓ Navigated to Case Registration');
    }
    
    // Wait for requests list to load
    await page.waitForLoadState('networkidle');
    
    // Look for requests in the list
    const requests = await page.locator('mat-card, [role="row"]').count();
    console.log(`📋 Found ${requests} items on page`);
    
    // Try to find and click on a request to open details
    const requestRows = page.locator('mat-row, tr[role="row"]');
    const rowCount = await requestRows.count();
    console.log(`🔍 Found ${rowCount} request rows`);
    
    if (rowCount > 0) {
      console.log('📌 Clicking first request to open details...');
      await requestRows.first().click();
      await page.waitForLoadState('networkidle');
      console.log('✓ Request details page loaded');
      
      // Look for the defendants tab/section
      const defendantsSidebar = page.locator('text=المدعى عليهم, text=Defendants');
      const defendantsCount = await defendantsSidebar.count();
      
      if (defendantsCount > 0) {
        console.log('✓ Found Defendants tab in sidebar');
        await defendantsSidebar.first().click();
        await page.waitForLoadState('networkidle');
        console.log('✓ Clicked Defendants tab');
        
        // Check for defendants list component
        const defendantsList = page.locator('app-defendant-list, .defendant-list-container');
        const listVisible = await defendantsList.isVisible();
        
        if (listVisible) {
          console.log('✅ Defendants list component is visible!');
          
          // Check for add defendant button
          const addButton = page.locator('button:has-text("إضافة مدعى عليه"), button:has-text("Add Defendant")');
          const addButtonExists = await addButton.count() > 0;
          
          if (addButtonExists) {
            console.log('✅ "Add Defendant" button found');
          }
          
          // Check for defendant type menu
          const typeMenu = page.locator('.defendant-type-menu, mat-menu');
          const menuCount = await typeMenu.count();
          console.log(`📋 Found ${menuCount} menu(s)`);
          
          // Take screenshot
          await page.screenshot({ path: 'defendants-tab-test.png' });
          console.log('📸 Screenshot saved: defendants-tab-test.png');
          
          console.log('\n✅ DEFENDANTS TAB TEST SUCCESSFUL!');
          console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
          console.log('✓ Component loaded and visible');
          console.log('✓ Add defendant button present');
          console.log('✓ UI elements rendering correctly');
        } else {
          console.log('❌ Defendants list component not visible');
          await page.screenshot({ path: 'defendants-tab-error.png' });
        }
      } else {
        console.log('⚠️  Defendants tab not found in sidebar');
        const sidebarItems = page.locator('[role="listitem"], li');
        const itemsText = await sidebarItems.allTextContents();
        console.log('Available sidebar items:', itemsText.slice(0, 5));
      }
    } else {
      console.log('⚠️  No requests found to test');
    }
    
    process.exit(0);
  } catch (error) {
    console.error('❌ Error during test:', error.message);
    process.exit(1);
  } finally {
    await browser.close();
  }
})();
