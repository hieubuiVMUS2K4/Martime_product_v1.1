import { toast } from 'sonner';

export interface FormSubmitOptions<T> {
  onSubmit: (data: T) => Promise<any>;
  onSuccess?: (result: any) => void;
  onError?: (error: any) => void;
  clearDraft?: () => Promise<void> | void;
  setIsSubmitting?: (isSubmitting: boolean) => void;
}

/**
 * Executes a form submission with double-submit protection, error recovery, and auto-draft cleanup.
 */
export async function handleSafeFormSubmit<T>(
  data: T,
  options: FormSubmitOptions<T>
): Promise<boolean> {
  const { onSubmit, onSuccess, onError, clearDraft, setIsSubmitting } = options;

  try {
    if (setIsSubmitting) setIsSubmitting(true);

    const result = await onSubmit(data);

    // Submission succeeded -> clear draft and notify user
    if (clearDraft) {
      await clearDraft();
    }

    if (onSuccess) {
      onSuccess(result);
    }

    return true;
  } catch (error: any) {
    console.error('Safe Form Submit Failed:', error);

    const errorMessage =
      error?.message === 'Failed to fetch' || error?.status === 0
        ? '⚠️ Không thể kết nối tới máy chủ. Dữ liệu của bạn vẫn được giữ an toàn trên màn hình, vui lòng kiểm tra kết nối và thử lại!'
        : error?.message || 'Có lỗi xảy ra khi lưu dữ liệu. Vui lòng kiểm tra lại!';

    toast.error(errorMessage);

    if (onError) {
      onError(error);
    }

    return false;
  } finally {
    if (setIsSubmitting) setIsSubmitting(false);
  }
}
