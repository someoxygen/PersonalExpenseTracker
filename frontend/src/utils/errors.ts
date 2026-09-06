import axios from "axios";
interface Problem {
  title?: string;
  errors?: Record<string, string[]>;
}
export function errorMessage(error: unknown): string {
  if (!axios.isAxiosError<Problem>(error))
    return "İşlem tamamlanamadı. Lütfen tekrar deneyin.";
  const status = error.response?.status;
  if (!status)
    return "Sunucuya ulaşılamıyor. Bağlantınızı kontrol edip tekrar deneyin.";
  if (status >= 500) return "Sunucuda bir sorun oluştu. Lütfen tekrar deneyin.";
  if (status === 401)
    return "Oturum veya giriş bilgileri geçersiz. Lütfen yeniden giriş yapın.";
  if (status === 403) return "Bu işlem için yetkiniz yok.";
  if (status === 404) return "Kayıt bulunamadı veya erişiminiz yok.";
  if (status === 429)
    return "Çok fazla deneme yapıldı. Bir dakika sonra tekrar deneyin.";
  const problem = error.response?.data;
  if (problem?.errors) return Object.values(problem.errors).flat().join(" ");
  return problem?.title ?? "İşlem tamamlanamadı.";
}
