import { test, expect, type Page } from '@playwright/test';

const qrCodesResponse = {
  items: [
    {
      id: '1',
      displayName: 'Demo QR Code',
      shortCode: 'ABC123',
      redirectUrl: 'https://example.com',
      updatedAt: '2024-01-01T00:00:00Z',
      createdAt: '2024-01-01T00:00:00Z',
      dotStyle: 'square',
      cornerDotStyle: 'square',
      cornerSquareStyle: 'square',
      color: '#000000',
    },
  ],
  currentPage: 1,
  pageSize: 20,
  totalCount: 1,
};

async function dismissViteOverlay(page: Page) {
  const overlay = page.locator('vite-plugin-checker-error-overlay');
  if (await overlay.count()) {
    await overlay.evaluateAll((nodes) => nodes.forEach((n) => n.remove()));
  }
}

async function authenticate(page: Page) {
  await page.goto('/login');
  await page.evaluate(() => {
    localStorage.setItem('accessToken', 'test-access-token');
    localStorage.setItem('refreshToken', 'test-refresh-token');
    window.location.href = '/qr-codes';
  });
  await page.waitForURL(/\/qr-codes/);
}

test.describe('authenticated flows', () => {
  test('renders QR codes list with server data', async ({ page }) => {
    await page.route('http://localhost:5097/**', async (route) => {
      const url = route.request().url();
      if (url.includes('/qr-codes') && route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(qrCodesResponse),
        });
        return;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({}),
      });
    });

    await authenticate(page);
    await dismissViteOverlay(page);

    await expect(page.getByText('Demo QR Code')).toBeVisible();
    await expect(page.getByRole('link', { name: 'Create QR Code' })).toBeVisible();
  });

  test('navigates to create QR code page from list', async ({ page }) => {
    await page.route('http://localhost:5097/**', async (route) => {
      const url = route.request().url();
      if (url.includes('/qr-codes') && route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(qrCodesResponse),
        });
        return;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({}),
      });
    });

    await authenticate(page);
    await dismissViteOverlay(page);

    await page.getByRole('link', { name: 'Create QR Code' }).click();
    await expect(page).toHaveURL(/\/qr-codes\/create$/);
    await expect(page.getByLabel('Display Name')).toBeVisible();
  });

  test('submits password change on account page', async ({ page }) => {
    let receivedBody: unknown;

    await page.route('http://localhost:5097/**', async (route) => {
      const url = route.request().url();
      if (url.endsWith('/auth/change-password')) {
        receivedBody = route.request().postDataJSON();
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({}),
        });
        return;
      }
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({}),
      });
    });

    await authenticate(page);
    await page.goto('/account');
    await dismissViteOverlay(page);

    await page.getByLabel('Current Password').fill('old-pass-123');
    await page.getByLabel('New Password').fill('new-pass-456');

    const [request] = await Promise.all([
      page.waitForRequest('**/auth/change-password'),
      page.getByRole('button', { name: 'Save' }).click(),
    ]);

    await expect(page.getByText('Password updated successfully.')).toBeVisible();

    expect(receivedBody).toMatchObject({
      currentPassword: 'old-pass-123',
      newPassword: 'new-pass-456',
    });
    expect(request.method()).toBe('POST');
  });
});
