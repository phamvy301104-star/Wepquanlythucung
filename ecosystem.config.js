module.exports = {
  apps: [
    {
      name: 'ume-pet-salon',
      script: './ume-backend/src/server.js',
      cwd: '/var/www/ume-pet-salon',
      instances: 1,
      autorestart: true,
      watch: false,
      max_memory_restart: '500M',
      env: {
        NODE_ENV: 'production',
        PORT: 5000
      },
      error_file: './logs/err.log',
      out_file: './logs/out.log',
      log_file: './logs/combined.log',
      time: true
    }
  ]
};
