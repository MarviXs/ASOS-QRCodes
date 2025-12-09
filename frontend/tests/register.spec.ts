import { test, expect } from '@playwright/test';

test('registers a new user', async ({ page }) => {
  const email = 'new-user@example.com';
  const password = 'StrongPass123!';

  let receivedBody: unknown;
  const dismissViteOverlay = async () => {
    const overlay = page.locator('vite-plugin-checker-error-overlay');
    if (await overlay.count()) {
      await overlay.evaluateAll((nodes) => nodes.forEach((n) => n.remove()));
    }
  };

  await page.route('**/auth/register', async (route) => {
    receivedBody = route.request().postDataJSON();
    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({ message: 'registered' }),
    });
  });

  await page.goto('/register');

  await page.getByLabel('Email Address').fill(email);
  await page.getByLabel('Password').fill(password);

  await dismissViteOverlay();

  const [request] = await Promise.all([
    page.waitForRequest('**/auth/register'),
    page.getByRole('button', { name: 'Sign Up' }).click(),
  ]);

  await expect(page.getByText('Registration successful! Please log in.')).toBeVisible();

  expect(receivedBody).toMatchObject({
    email,
    password,
  });

  expect(request.method()).toBe('POST');
});
