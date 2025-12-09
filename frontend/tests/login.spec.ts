import { test, expect, type Page } from '@playwright/test';

const credentials = {
  email: 'user@example.com',
  password: 'StrongPass123!',
};

async function dismissViteOverlay(page: Page) {
  const overlay = page.locator('vite-plugin-checker-error-overlay');
  if (await overlay.count()) {
    await overlay.evaluateAll((nodes) => nodes.forEach((n) => n.remove()));
  }
}

test.describe('login', () => {
  test('logs in with fixed credentials', async ({ page }) => {
    let receivedBody: unknown;

    await page.route('http://localhost:5097/**', async (route) => {
      const url = route.request().url();

      if (url.endsWith('/auth/login')) {
        receivedBody = route.request().postDataJSON();
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            accessToken: 'fake-access-token',
            refreshToken: 'fake-refresh-token',
          }),
        });
        return;
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({}),
      });
    });

    await page.goto('/login');

    await page.getByLabel('Email Address').fill(credentials.email);
    await page.getByLabel('Password').fill(credentials.password);

    await dismissViteOverlay(page);

    const [request] = await Promise.all([
      page.waitForRequest('**/auth/login'),
      page.getByRole('button', { name: 'Log In' }).click(),
    ]);

    await expect(page.getByText('Successfully logged in!')).toBeVisible();

    expect(receivedBody).toMatchObject({
      email: credentials.email,
      password: credentials.password,
    });

    expect(request.method()).toBe('POST');
  });

  test('shows an error toast on failed login', async ({ page }) => {
    await page.route('http://localhost:5097/**', async (route) => {
      const url = route.request().url();

      if (url.endsWith('/auth/login')) {
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({
            title: 'Invalid credentials',
          }),
        });
        return;
      }

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({}),
      });
    });

    await page.goto('/login');

    await page.getByLabel('Email Address').fill(credentials.email);
    await page.getByLabel('Password').fill('wrong-password');

    await dismissViteOverlay(page);

    await page.getByRole('button', { name: 'Log In' }).click();

    await expect(page.getByText('Invalid credentials')).toBeVisible();
  });

  test('redirects unauthenticated users to login when visiting home', async ({ page }) => {
    await page.goto('/');

    await dismissViteOverlay(page);

    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByRole('heading', { name: 'Login' })).toBeVisible();
  });
});
