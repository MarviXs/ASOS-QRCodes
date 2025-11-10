import { expect, test, type Page } from '@playwright/test';

const loginToastText = 'Successfully logged in!';
const registerToastText = 'Registration successful! Please log in.';
const accessTokenHeader = 'eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0';
const accessTokenPayload =
  'eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEyMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAu' +
  'b3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RAZXhhbXBsZS5jb20iLCJodHRwOi8vc2NoZWFtcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRp' +
  'HkvY2xhaW1zL3JvbGUiOiJBZG1pbiJ9';
const accessTokenSignature = 'c2lnbmF0dXJl';
const accessToken = `${accessTokenHeader}.${accessTokenPayload}.${accessTokenSignature}`;
const refreshToken = 'mock-refresh-token';

const scanAnalyticsFixture = {
  dailyScans: [],
  countries: [],
  operatingSystems: [],
  browsers: [],
  deviceTypes: [],
  totalScansInPeriod: 0,
  lifetimeScans: 0,
  startDate: '2024-01-01T00:00:00Z',
  endDate: '2024-01-01T00:00:00Z',
};

const scanRecordsFixture = {
  totalCount: 0,
  hasPrevious: false,
  hasNext: false,
  items: [],
};

async function mockProtectedEndpoints(page: Page) {
  await page.route('**/scan-records/analytics**', async (route) => {
    await route.fulfill({
      status: 200,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(scanAnalyticsFixture),
    });
  });

  await page.route('**/scan-records**', async (route) => {
    await route.fulfill({
      status: 200,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(scanRecordsFixture),
    });
  });
}

test.describe('Authentication views', () => {
  test('allows a user to log in', async ({ page }) => {
    let capturedRequestBody: Record<string, unknown> | undefined;

    await mockProtectedEndpoints(page);

    await page.route('**/auth/login', async (route) => {
      capturedRequestBody = route.request().postDataJSON() as Record<string, unknown>;
      await route.fulfill({
        status: 200,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ accessToken, refreshToken }),
      });
    });

    await page.goto('/login');
    await page.getByLabel('Email Address').fill('test@example.com');
    await page.getByLabel('Password').fill('SuperSecret123');
    await page.getByRole('button', { name: 'Log In' }).click();

    await expect(page.getByText(loginToastText)).toBeVisible();
    await expect(page).toHaveURL('http://127.0.0.1:8020/');

    const storedAccessToken = await page.evaluate(() => localStorage.getItem('accessToken'));
    expect(storedAccessToken).toBe(accessToken);
    expect(capturedRequestBody).toEqual({
      email: 'test@example.com',
      password: 'SuperSecret123',
    });
  });

  test('registers a new account and returns to login', async ({ page }) => {
    let capturedRequestBody: Record<string, unknown> | undefined;

    await page.route('**/auth/register', async (route) => {
      capturedRequestBody = route.request().postDataJSON() as Record<string, unknown>;
      await route.fulfill({
        status: 200,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({}),
      });
    });

    await page.goto('/register');
    await page.getByLabel('Email Address').fill('new-user@example.com');
    await page.getByLabel('Password').fill('AnotherSecret123');
    await page.getByRole('button', { name: 'Sign Up' }).click();

    await expect(page.getByText(registerToastText)).toBeVisible();
    await expect(page).toHaveURL('http://127.0.0.1:8020/login');
    expect(capturedRequestBody).toEqual({
      email: 'new-user@example.com',
      password: 'AnotherSecret123',
    });
  });
});
