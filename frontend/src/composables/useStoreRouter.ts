import { useDiStore } from '@/stores/di';

// Router for stores
export function useStoreRouter() {
  const di = useDiStore();
  return di.router;
}
