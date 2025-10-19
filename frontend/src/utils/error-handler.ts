import type { ProblemDetails } from '@/api/types/ProblemDetails';
import { toast } from 'vue3-toastify';

function handleError(error: ProblemDetails, defaultErrorMessage: string): unknown {
  let message = defaultErrorMessage;

  try {
    if (error && error.title) {
      message = error.title;

      if (error.errors) {
        const firstKey = Object.keys(error.errors)[0];
        if (firstKey && error.errors[firstKey]) {
          const firstErrorMessage = error.errors[firstKey][0];
          if (firstErrorMessage) {
            message = firstErrorMessage;
          }
        }
      }
    }
  } catch (e) {
    console.error(e);
  }

  if (message.length > 0) {
    toast.error(message);
  } else {
    console.error(error);
  }

  return error;
}

export { handleError };
