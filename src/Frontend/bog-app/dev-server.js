const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');
const http = require('http');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 4201;
const ANGULAR_DEV_SERVER = process.env.ANGULAR_DEV_SERVER || 'http://localhost:4202';

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ status: 'ok' });
});

// Proxy API requests directly to backend
app.use('/api', createProxyMiddleware({
  target: 'http://localhost:5001',
  changeOrigin: true,
  logLevel: 'warn'
}));

// For all other requests, proxy to Angular dev server with SPA fallback
app.all('*', (req, res) => {
  const isAsset = /\.(js|css|png|gif|jpg|jpeg|svg|ico|woff|woff2|ttf|eot|json|map)$/.test(req.path);

  if (isAsset) {
    // For assets, just proxy directly
    const proxy = createProxyMiddleware({
      target: ANGULAR_DEV_SERVER,
      changeOrigin: true,
      logLevel: 'warn'
    });
    proxy(req, res);
  } else {
    // For non-assets (HTML routes), try to fetch the requested path
    // If it 404s, fallback to index.html for SPA routing
    const targetUrl = `${ANGULAR_DEV_SERVER}${req.path}`;

    http.get(targetUrl.replace('http', 'http'), (targetRes) => {
      if (targetRes.statusCode === 404) {
        // Fallback to index.html
        http.get(`${ANGULAR_DEV_SERVER}/`, (indexRes) => {
          res.writeHead(200, indexRes.headers);
          indexRes.pipe(res);
        }).on('error', (err) => {
          res.status(500).send('Server error');
        });
      } else {
        res.writeHead(targetRes.statusCode, targetRes.headers);
        targetRes.pipe(res);
      }
    }).on('error', (err) => {
      // If connection fails, try index.html
      http.get(`${ANGULAR_DEV_SERVER}/`, (indexRes) => {
        res.writeHead(indexRes.statusCode, indexRes.headers);
        indexRes.pipe(res);
      }).on('error', (err) => {
        res.status(500).send('Server error');
      });
    });
  }
});

app.listen(PORT, () => {
  console.log(`SPA Wrapper Server running at http://localhost:${PORT}`);
  console.log(`Proxying Angular dev server at ${ANGULAR_DEV_SERVER}`);
  console.log(`Backend API at http://localhost:5000`);
});
