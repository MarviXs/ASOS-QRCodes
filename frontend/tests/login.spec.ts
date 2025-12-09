import { test, expect } from '@playwright/test';

test('logs in with fixed credentials', async ({ page }) => {
  const email = 'user@example.com';
  const password = 'StrongPass123!';

  let receivedBody: unknown;
  const dismissViteOverlay = async () => {
    const overlay = page.locator('vite-plugin-checker-error-overlay');
    if (await overlay.count()) {
      await overlay.evaluateAll((nodes) => nodes.forEach((n) => n.remove()));
    }
  };

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

  await page.getByLabel('Email Address').fill(email);
  await page.getByLabel('Password').fill(password);

  await dismissViteOverlay();

  const [request] = await Promise.all([
    page.waitForRequest('**/auth/login'),
    page.getByRole('button', { name: 'Log In' }).click(),
  ]);

  await expect(page.getByText('Successfully logged in!')).toBeVisible();

  expect(receivedBody).toMatchObject({
    email,
    password,
  });

  expect(request.method()).toBe('POST');
});
