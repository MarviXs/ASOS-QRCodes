import type { QField } from 'quasar';
import { describe, expect, it, vi } from 'vitest';

import { isFormValid } from '@/utils/form-validation';

const createField = (hasErrorAfterValidate = false): QField => {
  const field = {
    hasError: false,
    validate: vi.fn(() => {
      field.hasError = hasErrorAfterValidate;
    }),
  };

  return field as unknown as QField;
};

describe('isFormValid', () => {
  it('returns false when no inputs are provided', () => {
    expect(isFormValid(undefined)).toBe(false);
  });

  it('returns true when every field validates without errors', () => {
    const fields = [createField(false), createField(false)];

    expect(isFormValid(fields)).toBe(true);
    fields.forEach((field) => expect(field.validate).toHaveBeenCalled());
  });

  it('returns false when any field reports an error', () => {
    const fields = [createField(false), createField(true)];

    expect(isFormValid(fields)).toBe(false);
    fields.forEach((field) => expect(field.validate).toHaveBeenCalled());
  });
});
