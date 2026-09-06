import { onBeforeUnmount, ref, shallowRef } from "vue";
import { errorMessage } from "../utils/errors";
export function useResource<T>(fetcher: () => Promise<T>) {
  const data = shallowRef<T>();
  const loading = ref(false);
  const error = ref("");
  let version = 0;
  async function load() {
    const current = ++version;
    loading.value = true;
    error.value = "";
    try {
      const result = await fetcher();
      if (current === version) data.value = result;
    } catch (e) {
      if (current === version) error.value = errorMessage(e);
    } finally {
      if (current === version) loading.value = false;
    }
  }
  onBeforeUnmount(() => {
    version++;
  });
  return { data, loading, error, load };
}
