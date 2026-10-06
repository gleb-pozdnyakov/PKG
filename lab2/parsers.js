const fs = require('fs');
const path = require('path');

function parseBMP(filePath) {
    const fd = fs.openSync(filePath, 'r');
    const buffer = Buffer.alloc(54);
    fs.readSync(fd, buffer, 0, 54, 0);
    fs.closeSync(fd);

    if (buffer.readUInt16BE(0) !== 0x424D) {
        throw new Error("Invalid BMP signature");
    }

    const width = buffer.readInt32LE(18);
    const height = Math.abs(buffer.readInt32LE(22));
    const bpp = buffer.readUInt16LE(28);
    const compressionType = buffer.readUInt32LE(30);

    const compressions = { 0: "BI_RGB (Нет)", 1: "BI_RLE8", 2: "BI_RLE4", 3: "BI_BITFIELDS" };
    return {
        width, height, dpi: "72", bpp: `${bpp} bit`,
        compression: compressions[compressionType] || `Код: ${compressionType}`,
        status: "OK"
    };
}

function parsePNG(filePath) {
    const fd = fs.openSync(filePath, 'r');
    const buffer = Buffer.alloc(33);
    fs.readSync(fd, buffer, 0, 33, 0);
    fs.closeSync(fd);

    if (buffer.readUInt32BE(0) !== 0x89504E47 || buffer.readUInt32BE(4) !== 0x0D0A1A0A) {
        throw new Error("Invalid PNG signature");
    }

    if (buffer.readUInt32BE(12) !== 0x49484452) {
        throw new Error("IHDR chunk not found");
    }

    const width = buffer.readUInt32BE(16);
    const height = buffer.readUInt32BE(20);
    const bitDepth = buffer.readUInt8(24);
    const colorType = buffer.readUInt8(25);

    let bpp = bitDepth;
    if (colorType === 2) bpp = bitDepth * 3;
    if (colorType === 4) bpp = bitDepth * 2;
    if (colorType === 6) bpp = bitDepth * 4;

    const stats = fs.statSync(filePath);
    if (stats.size > 12) {
        const endFd = fs.openSync(filePath, 'r');
        const endBuffer = Buffer.alloc(12);
        fs.readSync(endFd, endBuffer, 0, 12, stats.size - 12);
        fs.closeSync(endFd);
        if (endBuffer.toString('hex', 4, 8) !== '49454e44') {
            return { width, height, dpi: "72", bpp: `${bpp} bit`, compression: "Deflate (zlib) + PNG Фильтрация", status: "Поврежден (Нет IEND)" };
        }
    }

    return { width, height, dpi: "72", bpp: `${bpp} bit`, compression: "Deflate (zlib) + PNG Фильтрация", status: "OK" };
}

function parseJPEG(filePath) {
    const stats = fs.statSync(filePath);
    const fd = fs.openSync(filePath, 'r');
    
    const startBuf = Buffer.alloc(2);
    fs.readSync(fd, startBuf, 0, 2, 0);
    if (startBuf.readUInt8(0) !== 0xFF || startBuf.readUInt8(1) !== 0xD8) {
        fs.closeSync(fd);
        throw new Error("Invalid JPEG signature");
    }

    const endBuf = Buffer.alloc(2);
    fs.readSync(fd, endBuf, 0, 2, stats.size - 2);
    let isCorrupted = (endBuf.readUInt8(0) !== 0xFF || endBuf.readUInt8(1) !== 0xD9);

    let offset = 2;
    let width = 0, height = 0, bpp = 24, dpiValue = "72", estimatedQuality = "Неизвестно";
    const markerBuf = Buffer.alloc(4);

    while (offset < stats.size - 10) {
        fs.readSync(fd, markerBuf, 0, 4, offset);
        
        if (markerBuf.readUInt8(0) !== 0xFF) {
            isCorrupted = true;
            break;
        }

        const marker = markerBuf.readUInt8(1);
        const length = markerBuf.readUInt16BE(2);

        if (marker === 0xDB) {
            const dqtBuf = Buffer.alloc(2);
            fs.readSync(fd, dqtBuf, 0, 2, offset + 4);
            const firstDqtValue = dqtBuf.readUInt8(1); 
            
            if (firstDqtValue > 0) {
                let q = 0;
                if (firstDqtValue < 16) {
                    q = Math.round(100 - (firstDqtValue * 50) / 16);
                } else {
                    q = Math.round((100 * 50) / firstDqtValue);
                }
                if (q >= 1 && q <= 100) estimatedQuality = `${q}%`;
            }
        }

        if (marker === 0xE0) {
            const app0Buf = Buffer.alloc(14);
            fs.readSync(fd, app0Buf, 0, 14, offset + 4);
            if (app0Buf.toString('utf8', 0, 4) === 'JFIF') {
                const xDensity = app0Buf.readUInt16BE(8);
                if (xDensity > 0) dpiValue = `${xDensity}`;
            }
        }

        if (marker === 0xC0 || marker === 0xC2) {
            const sofBuf = Buffer.alloc(9);
            fs.readSync(fd, sofBuf, 0, 9, offset + 4);
            height = sofBuf.readUInt16BE(1);
            width = sofBuf.readUInt16BE(3);
            const components = sofBuf.readUInt8(5);
            bpp = components * 8;
            break;
        }

        offset += 2 + length;
    }

    fs.closeSync(fd);

    if (width === 0 || height === 0) {
        throw new Error("SOF marker not found");
    }

    return {
        width, height, dpi: dpiValue, bpp: `${bpp} bit`,
        compression: `JPEG (Lossy ДКП, Качество: ${estimatedQuality})`,
        status: isCorrupted ? "Поврежден (Нет EOI)" : "OK"
    };
}



function parseGIF(filePath) {
    const fd = fs.openSync(filePath, 'r');
    const buffer = Buffer.alloc(13);
    fs.readSync(fd, buffer, 0, 13, 0);
    fs.closeSync(fd);

    const sig = buffer.toString('utf8', 0, 6);
    if (sig !== 'GIF87a' && sig !== 'GIF89a') {
        throw new Error("Invalid GIF signature");
    }

    const width = buffer.readUInt16LE(6);
    const height = buffer.readUInt16LE(8);
    const packed = buffer.readUInt8(10);
    const hasGlobalPalette = (packed & 0x80) !== 0;
    const sizeOfGlobalPalette = packed & 0x07;
    const numColors = hasGlobalPalette ? Math.pow(2, sizeOfGlobalPalette + 1) : 0;

    return {
        width, height, dpi: "72", bpp: "8 bit (Индексированный)",
        compression: `LZW (Цветов в палитре: ${numColors})`, status: "OK"
    };
}

function parseTIFF(filePath) {
    const fd = fs.openSync(filePath, 'r');
    const header = Buffer.alloc(8);
    fs.readSync(fd, header, 0, 8, 0);

    const isLittleEndian = header.toString('utf8', 0, 2) === 'II';
    if (!isLittleEndian && header.toString('utf8', 0, 2) !== 'MM') {
        fs.closeSync(fd);
        throw new Error("Invalid TIFF signature");
    }

    const magic = isLittleEndian ? header.readUInt16LE(2) : header.readUInt16BE(2);
    if (magic !== 42) {
        fs.closeSync(fd);
        throw new Error("Invalid TIFF magic number");
    }

    let ifdOffset = isLittleEndian ? header.readUInt32LE(4) : header.readUInt32BE(4);
    
    const countBuf = Buffer.alloc(2);
    fs.readSync(fd, countBuf, 0, 2, ifdOffset);
    const numEntries = isLittleEndian ? countBuf.readUInt16LE(0) : countBuf.readUInt16BE(0);

    let width = "-", height = "-", bpp = "-", compCode = "-", dpiValue = "72";
    const entryBuf = Buffer.alloc(12);

    for (let i = 0; i < numEntries; i++) {
        fs.readSync(fd, entryBuf, 0, 12, ifdOffset + 2 + (i * 12));
        const tag = isLittleEndian ? entryBuf.readUInt16LE(0) : entryBuf.readUInt16BE(0);
        const type = isLittleEndian ? entryBuf.readUInt16LE(2) : entryBuf.readUInt16BE(2);
        
        let value = 0;
        if (isLittleEndian) {
            value = entryBuf.readUInt16LE(8);
            if (tag === 256 || tag === 257 || tag === 282) value = entryBuf.readUInt32LE(8) || entryBuf.readUInt16LE(8);
        } else {
            value = entryBuf.readUInt16BE(8);
            if (tag === 256 || tag === 257 || tag === 282) value = entryBuf.readUInt32BE(8) || entryBuf.readUInt16BE(8);
        }

        if (tag === 256) width = value; 
        if (tag === 257) height = value;
        if (tag === 259) compCode = value;

        if (tag === 282) {
            if (type === 5) {
                const ratBuf = Buffer.alloc(8);
                fs.readSync(fd, ratBuf, 0, 8, value);         
                const num = isLittleEndian ? ratBuf.readUInt32LE(0) : ratBuf.readUInt32BE(0);
                const den = isLittleEndian ? ratBuf.readUInt32LE(4) : ratBuf.readUInt32BE(4);
                if (den > 0) {
                    dpiValue = Math.round(num / den).toString();
                }
            } else {
                dpiValue = value.toString();
            }
        }
        
        if (tag === 258) bpp = value > 32 ? "24 bit (RGB)" : `${value} bit`;
    }
    fs.closeSync(fd);

    const tiffComp = { 1: "None (Без сжатия)", 5: "LZW (Без потерь)", 6: "JPEG (С потерями)", 8: "Deflate (Без потерь)", 32773: "PackBits" };
    return {
        width, height, dpi: dpiValue, bpp,
        compression: tiffComp[compCode] || `Код сжатия: ${compCode}`, status: "OK"
    };
}


function parsePCX(filePath) {
    const fd = fs.openSync(filePath, 'r');
    const header = Buffer.alloc(128);
    fs.readSync(fd, header, 0, 128, 0);
    fs.closeSync(fd);

    if (header.readUInt8(0) !== 0x0A) {
        throw new Error("Invalid PCX signature");
    }

    const version = header.readUInt8(1);
    const encoding = header.readUInt8(2) === 1 ? "RLE (Без потерь)" : "Без сжатия";
    const bitsPerPixel = header.readUInt8(3);
    
    const xMin = header.readUInt16LE(4);  const yMin = header.readUInt16LE(6);
    const xMax = header.readUInt16LE(8);  const yMax = header.readUInt16LE(10);
    
    const hdpi = header.readUInt16LE(12);
    const vdpi = header.readUInt16LE(14);
    const dpi = hdpi === vdpi ? `${hdpi}` : `${hdpi}x${vdpi}`;

    const width = xMax - xMin + 1;
    const height = yMax - yMin + 1;
    const planes = header.readUInt8(65);
    const bpp = bitsPerPixel * planes;

    const pcxVersions = { 0: "v2.5", 2: "v2.8 с палитрой", 3: "v2.8 без палитры", 5: "v3.0 (24-bit)" };

    return {
        width, height, dpi: dpi > 0 ? dpi : "300", bpp: `${bpp} bit`,
        compression: `${encoding} [PCX ${pcxVersions[version] || 'v' + version}]`, status: "OK"
    };
}

function parseImageFile(filePath) {
    const ext = path.extname(filePath).toLowerCase();
    try {
        if (ext === '.bmp') return parseBMP(filePath);
        if (ext === '.png') return parsePNG(filePath);
        if (ext === '.jpg' || ext === '.jpeg') return parseJPEG(filePath);
        if (ext === '.gif') return parseGIF(filePath);
        if (ext === '.tif' || ext === '.tiff') return parseTIFF(filePath);
        if (ext === '.pcx') return parsePCX(filePath);
        return null;
    } catch (err) {
        return { width: "-", height: "-", dpi: "-", bpp: "-", compression: "-", status: "Поврежден / Ошибка" };
    }
}

module.exports = { parseImageFile };
