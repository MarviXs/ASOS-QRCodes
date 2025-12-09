import { describe, expect, it, vi } from 'vitest';

vi.mock('vue3-toastify', () => {
  const error = vi.fn();
  return { toast: { error } };
});

import type { ProblemDetails } from '@/api/types/ProblemDetails';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';

describe('handleError', () => {
  it('shows the first validation error when available', () => {
    const error = {
      title: 'Validation failed',
      errors: {
        name: ['Name is required', 'Another message'],
        email: ['Invalid'],
      },
    } as unknown as ProblemDetails;

    handleError(error, 'Default error');

    expect(toast.error).toHaveBeenCalledWith('Name is required');
  });

  it('falls back to the title when no field errors exist', () => {
    const error = {
      title: 'Action failed',
    } as unknown as ProblemDetails;

    handleError(error, 'Default error');

    expect(toast.error).toHaveBeenCalledWith('Action failed');
  });

  it('logs the error when no message can be shown', () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => { });

    handleError({} as unknown as ProblemDetails, '');

    expect(consoleSpy).toHaveBeenCalledWith({});
    expect(toast.error).not.toHaveBeenCalled();
  });
});
