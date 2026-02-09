/// <reference types="cypress" />

import './commands';

// Suppress uncaught exceptions from the Angular app
Cypress.on('uncaught:exception', () => false);
