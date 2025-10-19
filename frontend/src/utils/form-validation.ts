import type { QField } from 'quasar';

function isFormValid(inputRefs: (QField | undefined)[]) {
  if (!inputRefs) return false;
  let hasError = false;
  for (const input of inputRefs) {
    if (input) {
      input.validate();
      if (input.hasError) {
        hasError = true;
      }
    }
  }
  return !hasError;
}

export { isFormValid };
