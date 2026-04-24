import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

// ── HTTPS dev-cert (chỉ dùng khi chạy local với ASP.NET backend) ──────────
// Trên Vercel/CI, không có dotnet CLI nên skip hoàn toàn phần cert.
let httpsConfig = undefined;

const isCI = env.CI === 'true' || env.VERCEL === '1';

if (!isCI) {
  const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
      ? `${env.APPDATA}/ASP.NET/https`
      : `${env.HOME}/.aspnet/https`;

  const certificateName = 'cls.client';
  const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
  const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

  if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
  }

  if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
    if (
      0 !==
      child_process.spawnSync(
        'dotnet',
        [
          'dev-certs',
          'https',
          '--export-path',
          certFilePath,
          '--format',
          'Pem',
          '--no-password',
        ],
        { stdio: 'inherit' }
      ).status
    ) {
      // Không throw — cho phép build tiếp mà không có cert
      console.warn('⚠️  Could not create dev certificate. HTTPS disabled for dev server.');
    }
  }

  if (fs.existsSync(certFilePath) && fs.existsSync(keyFilePath)) {
    httpsConfig = {
      key: fs.readFileSync(keyFilePath),
      cert: fs.readFileSync(certFilePath),
    };
  }
}

const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(';')[0]
    : 'https://localhost:7065';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin(), tailwindcss()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    proxy: {
      '^/weatherforecast': {
        target,
        secure: false,
      },
      // Proxy toàn bộ /api/* sang backend — tránh mixed-content & SSL cert issues
      '^/api': {
        target,
        secure: false,
        changeOrigin: true,
      },
    },
    port: parseInt(env.DEV_SERVER_PORT || '57264'),
    https: httpsConfig,
  },
});
