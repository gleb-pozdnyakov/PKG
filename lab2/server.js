const http = require('http');
const fs = require('fs');
const path = require('path');
const { scanDirectory } = require('./engine'); // Импорт из Core Layer

const PORT = 3000;

const server = http.createServer((req, res) => {
    if (req.url === '/' || req.url === '/index.html') {
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end(fs.readFileSync(path.join(__dirname, 'index.html')));
        return;
    }

    if (req.url === '/style.css') {
        const cssPath = path.join(__dirname, 'style.css');
        if (fs.existsSync(cssPath)) {
            res.writeHead(200, { 'Content-Type': 'text/css; charset=utf-8' });
            res.end(fs.readFileSync(cssPath));
        } else {
            res.writeHead(404, { 'Content-Type': 'text/plain' });
            res.end('CSS Not Found');
        }
        return;
    }

    if (req.url.startsWith('/scan')) {
        const urlObj = new URL(req.url, `http://${req.headers.host}`);
        const folderPath = urlObj.searchParams.get('path');

        res.writeHead(200, {
            'Content-Type': 'text/event-stream',
            'Cache-Control': 'no-cache',
            'Connection': 'keep-alive'
        });

        scanDirectory(folderPath, res);
        return;
    }

    res.writeHead(404, { 'Content-Type': 'text/plain' });
    res.end('404 Not Found');
});

server.listen(PORT, () => {
    console.log(`\x1b[32m%s\x1b[0m`, `[ОК] Сервер запущен.`);
    console.log(`Откройте архитектурный UI: http://localhost:${PORT}`);
});
