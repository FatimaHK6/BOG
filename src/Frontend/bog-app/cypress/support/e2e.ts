// Cypress E2E Support File
// This runs before each test

// Suppress specific error messages
const app = window.top;

if (!app.document.head.querySelector('[data-hide-command-log-request]')) {
  const style = app.document.createElement('style');
  style.innerHTML =
    '.command-name-request, .command-name-xhr { display: none }';
  style.setAttribute('data-hide-command-log-request', '');

  app.document.head.appendChild(style);
}

// Handle uncaught exceptions
Cypress.on('uncaught:exception', (err, runnable) => {
  // Returning false here prevents Cypress from failing the test
  return false;
});

// Custom commands
Cypress.Commands.add('login', () => {
  // Add login logic if needed
  cy.visit('/');
});

// Override cy.log to use console.log
Cypress.Commands.overwrite('log', (originalFn, ...args) => {
  console.log(...args);
});
