import { test, expect } from '@playwright/test';

test('registers a new user with random credentials', async ({ page }) => {
  const randomEmail = `user_${Date.now()}_${Math.random().toString(36).slice(2, 8)}@example.com`;
  const randomPassword = `Pw!${Math.random().toString(36).slice(2, 10)}`;

  let receivedBody: unknown;

  await page.route('**/auth/register', async (route) => {
    receivedBody = route.request().postDataJSON();
    await route.fulfill({
      status: 201,
      contentType: 'application/json',
      body: JSON.stringify({ message: 'registered' }),
    });
  });

  await page.goto('/register');

  await page.getByLabel('Email Address').fill(randomEmail);
  await page.getByLabel('Password').fill(randomPassword);

  const [request] = await Promise.all([
    page.waitForRequest('**/auth/register'),
    page.getByRole('button', { name: 'Sign Up' }).click(),
  ]);

  await expect(page.getByText('Registration successful! Please log in.')).toBeVisible();

  expect(receivedBody).toMatchObject({
    email: randomEmail,
    password: randomPassword,
  });

  expect(request.method()).toBe('POST');
});
