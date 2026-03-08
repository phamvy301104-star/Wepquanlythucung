module.exports = {
  apps: [
    {
      name: 'ume-pet-salon',
      script: 'src/server.js',
      cwd: '/var/www/ume-pet-salon/ume-backend',
      instances: 1,
      exec_mode: 'fork',
      autorestart: true,
      watch: false,
      max_memory_restart: '1G',
      env: {
        NODE_ENV: 'production',
        PORT: 5000
      },
      error_file: '../logs/err.log',
      out_file: '../logs/out.log',
      log_file: '../logs/combined.log',
      time: true
    }
  ]
};
