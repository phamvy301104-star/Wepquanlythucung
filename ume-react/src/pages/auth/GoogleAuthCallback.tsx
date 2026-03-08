import { useEffect } from 'react';

export default function GoogleAuthCallback() {
  useEffect(() => {
    try {
      const hash = window.location.hash.substring(1);
      if (hash) {
        const data = JSON.parse(atob(hash));
        localStorage.setItem('google-auth-result', JSON.stringify(data));
      }
    } catch (e) {
      localStorage.setItem('google-auth-result', JSON.stringify({ success: false, error: 'parse_error' }));
    }
    window.close();
  }, []);

  return <p style={{ textAlign: 'center', marginTop: 40 }}>Đang xử lý đăng nhập...</p>;
}
