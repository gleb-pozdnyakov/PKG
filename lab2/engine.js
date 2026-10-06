const fs = require('fs');
const path = require('path');
const { parseImageFile } = require('./parsers');

async function scanDirectory(folderPath, res) {
    const cleanPath = folderPath.replace(/^["']|["']\$/g, '').trim();

    console.log(`\n--- [ДИАГНОСТИКА] Начало сканирования папки ---`);
    console.log(`Получен путь от UI: "${folderPath}"`);
    console.log(`Очищенный путь:     "${cleanPath}"`);

    if (!fs.existsSync(cleanPath)) {
        console.log(`[ОШИБКА] Путь не существует на диске!`);
        res.write(`data: ${JSON.stringify({ error: "Папка не существует" })}\n\n`);
        res.end();
        return;
    }

    let files;
    try {
        files = fs.readdirSync(cleanPath);
    } catch (e) {
        console.log(`[ОШИБКА] Не удалось прочитать папку:`, e.message);
        res.write(`data: ${JSON.stringify({ error: "Ошибка доступа к папке" })}\n\n`);
        res.end();
        return;
    }

    const totalFiles = files.length;
    console.log(`Всего объектов найдено через fs.readdirSync: ${totalFiles}`);
    console.log(`Список первых 10 объектов в этой папке:`, files.slice(0, 10));

    res.write(`data: ${JSON.stringify({ type: "start", total: totalFiles })}\n\n`);

    let processed = 0;
    const CHUNK_SIZE = 50;

    for (let i = 0; i < totalFiles; i += CHUNK_SIZE) {
        const chunk = files.slice(i, i + CHUNK_SIZE);
        
        const results = chunk.map(file => {
            const fullPath = path.join(cleanPath, file);
            try {
                const stat = fs.statSync(fullPath);
                if (stat.isFile()) {
                    const meta = parseImageFile(fullPath);
                    
                    console.log(`Файл: "${file}" -> Результат парсинга:`, meta);
                    
                    if (meta) {
                        return { name: file, ...meta };
                    }
                } else {
                    console.log(`Объект "${file}" пропущен, так как это папка.`);
                }
            } catch (e) {
                console.log(`Ошибка при обработке файла "${file}":`, e.message);
            }
            return null;
        }).filter(Boolean);

        processed += chunk.length;

        console.log(`Отправка пачки в UI. Найдено валидных картинок в пачке: ${results.length}`);

        res.write(`data: ${JSON.stringify({ type: "data", chunk: results, processed, total: totalFiles })}\n\n`);
        await new Promise(resolve => setImmediate(resolve));
    }

    console.log(`--- [ДИАГНОСТИКА] Сканирование завершено успешно ---\n`);
    res.write(`data: ${JSON.stringify({ type: "end" })}\n\n`);
    res.end();
}

module.exports = { scanDirectory };
