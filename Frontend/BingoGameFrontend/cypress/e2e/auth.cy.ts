describe('Auth Flow', () => {
  beforeEach(() => {
    cy.visit('/login');
  });

  it('should login with valid credentials', () => {
    cy.get('input[formControlName="usernameOrEmail"]').type('testuser');
    cy.get('input[formControlName="password"]').type('TestPass123');
    cy.get('button[type="submit"]').click();
    cy.url().should('include', '/dashboard');
  });

  it('should register new user', () => {
    cy.visit('/register');
    cy.get('input[formControlName="username"]').type('newuser');
    cy.get('input[formControlName="email"]').type('new@example.com');
    cy.get('input[formControlName="password"]').type('NewPass123');
    cy.get('button[type="submit"]').click();
    cy.url().should('include', '/dashboard');
  });
});