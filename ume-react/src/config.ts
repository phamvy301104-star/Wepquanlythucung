const config = {
  apiUrl: import.meta.env.VITE_API_URL || (import.meta.env.PROD ? '/api' : 'http://localhost:5000/api'),
  socketUrl: import.meta.env.VITE_SOCKET_URL || (import.meta.env.PROD ? '' : 'http://localhost:5000'),
  googleClientId: '871767057324-2uqmsqa07def6kul8g42f72qah39va6l.apps.googleusercontent.com',
};

export const getBaseUrl = () => config.apiUrl.replace('/api', '');

export default config;
