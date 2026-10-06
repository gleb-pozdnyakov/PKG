class ColorModel {
    constructor() {
        this.primaries = {
            r: { x: 0.64, y: 0.33 },
            g: { x: 0.30, y: 0.60 },
            b: { x: 0.15, y: 0.06 }
        };

        this.whitePoints = {
            'D65': { x: 0.3127, y: 0.3290, Xn: 95.047, Yn: 100.0, Zn: 108.883 },
            'D50': { x: 0.3457, y: 0.3585, Xn: 96.422, Yn: 100.0, Zn: 82.521 },
            'E':   { x: 1/3,    y: 1/3,    Xn: 100.0,  Yn: 100.0, Zn: 100.0 }
        };

        this.currentIlluminant = 'D65';
        this.gamutStrategy = 'clipping';

        this.updateMatrices();
    }

    updateMatrices() {
        const wp = this.whitePoints[this.currentIlluminant];
        
        const W = [wp.x / wp.y, 1.0, (1.0 - wp.x - wp.y) / wp.y];
        const A = [
            [this.primaries.r.x / this.primaries.r.y, this.primaries.g.x / this.primaries.g.y, this.primaries.b.x / this.primaries.b.y],
            [1.0, 1.0, 1.0],
            [(1.0 - this.primaries.r.x - this.primaries.r.y) / this.primaries.r.y, (1.0 - this.primaries.g.x - this.primaries.g.y) / this.primaries.g.y, (1.0 - this.primaries.b.x - this.primaries.b.y) / this.primaries.b.y]
        ];

        const detA = A[0][0]*(A[1][1]*A[2][2] - A[1][2]*A[2][1]) -
                     A[0][1]*(A[1][0]*A[2][2] - A[1][2]*A[2][0]) +
                     A[0][2]*(A[1][0]*A[2][1] - A[1][1]*A[2][0]);

        const invA = [
            [
                (A[1][1]*A[2][2] - A[1][2]*A[2][1]) / detA,
                (A[0][2]*A[2][1] - A[0][1]*A[2][2]) / detA,
                (A[0][1]*A[1][2] - A[0][2]*A[1][1]) / detA
            ],
            [
                (A[1][2]*A[2][0] - A[1][0]*A[2][2]) / detA,
                (A[0][0]*A[2][2] - A[0][2]*A[2][0]) / detA,
                (A[0][2]*A[1][0] - A[0][0]*A[1][2]) / detA
            ],
            [
                (A[1][0]*A[2][1] - A[1][1]*A[2][0]) / detA,
                (A[0][1]*A[2][0] - A[0][0]*A[2][1]) / detA,
                (A[0][0]*A[1][1] - A[0][1]*A[1][0]) / detA
            ]
        ];

        const S = [
            invA[0][0]*W[0] + invA[0][1]*W[1] + invA[0][2]*W[2],
            invA[1][0]*W[0] + invA[1][1]*W[1] + invA[1][2]*W[2],
            invA[2][0]*W[0] + invA[2][1]*W[1] + invA[2][2]*W[2]
        ];

        this.M = [
            [S[0]*A[0][0], S[1]*A[0][1], S[2]*A[0][2]],
            [S[0]*A[1][0], S[1]*A[1][1], S[2]*A[1][2]],
            [S[0]*A[2][0], S[1]*A[2][1], S[2]*A[2][2]]
        ];

        const detM = this.M[0][0]*(this.M[1][1]*this.M[2][2] - this.M[1][2]*this.M[2][1]) -
                     this.M[0][1]*(this.M[1][0]*this.M[2][2] - this.M[1][2]*this.M[2][0]) +
                     this.M[0][2]*(this.M[1][0]*this.M[2][1] - this.M[1][1]*this.M[2][0]);

        this.invM = [
            [
                (this.M[1][1]*this.M[2][2] - this.M[1][2]*this.M[2][1]) / detM,
                (this.M[0][2]*this.M[2][1] - this.M[0][1]*this.M[2][2]) / detM,
                (this.M[0][1]*this.M[1][2] - this.M[0][2]*this.M[1][1]) / detM
            ],
            [
                (this.M[1][2]*this.M[2][0] - this.M[1][0]*this.M[2][2]) / detM,
                (this.M[0][0]*this.M[2][2] - this.M[0][2]*this.M[2][0]) / detM,
                (this.M[0][2]*this.M[1][0] - this.M[0][0]*this.M[1][2]) / detM
            ],
            [
                (this.M[1][0]*this.M[2][1] - this.M[1][1]*this.M[2][0]) / detM,
                (this.M[0][1]*this.M[2][0] - this.M[0][0]*this.M[2][1]) / detM,
                (this.M[0][0]*this.M[1][1] - this.M[0][1]*this.M[1][0]) / detM
            ]
        ];
    }

    toLinear(x) {
        return x >= 0.04045 ? Math.pow((x + 0.055) / 1.055, 2.4) : x / 12.92;
    }

    toSRGB(x) {
        return x > 0.0031308 ? 1.055 * Math.pow(x, 1 / 2.4) - 0.055 : 12.92 * x;
    }

    handleGamut(rLinear, gLinear, bLinear) {
        let r = this.toSRGB(rLinear) * 255;
        let g = this.toSRGB(gLinear) * 255;
        let b = this.toSRGB(bLinear) * 255;

        const isOutOfGamut = r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255;

        if (isOutOfGamut) {
            if (this.gamutStrategy === 'clipping') {
                r = Math.max(0, Math.min(255, r));
                g = Math.max(0, Math.min(255, g));
                b = Math.max(0, Math.min(255, b));
            } else if (this.gamutStrategy === 'scaling') {
                const minVal = Math.min(r, g, b);
                const maxVal = Math.max(r, g, b);
                
                let low = minVal < 0 ? minVal : 0;
                let high = maxVal > 255 ? maxVal : 255;
                const range = high - low;
                
                if (range > 0) {
                    r = ((r - low) / range) * 255;
                    g = ((g - low) / range) * 255;
                    b = ((b - low) / range) * 255;
                }
            }
        }

        return { rgb: [Math.round(r), Math.round(g), Math.round(b)], clipped: isOutOfGamut };
    }

    rgbToHsv(r, g, b) {
        r /= 255; g /= 255; b /= 255;
        const max = Math.max(r, g, b), min = Math.min(r, g, b);
        const d = max - min;
        let h = 0, s = max === 0 ? 0 : d / max, v = max;

        if (max !== min) {
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }
            h *= 60;
        }
        return [Math.round(h), Math.round(s * 100), Math.round(v * 100)];
    }

    hsvToRgb(h, s, v) {
        h /= 360; s /= 100; v /= 100;
        let r, g, b;
        const i = Math.floor(h * 6);
        const f = h * 6 - i;
        const p = v * (1 - s);
        const q = v * (1 - f * s);
        const t = v * (1 - (1 - f) * s);

        switch (i % 6) {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            case 5: r = v; g = p; b = q; break;
        }
        return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
    }

    rgbToLab(r, g, b) {
        const rl = this.toLinear(r / 255) * 100;
        const gl = this.toLinear(g / 255) * 100;
        const bl = this.toLinear(b / 255) * 100;

        const X = this.M[0][0]*rl + this.M[0][1]*gl + this.M[0][2]*bl;
        const Y = this.M[1][0]*rl + this.M[1][1]*gl + this.M[1][2]*bl;
        const Z = this.M[2][0]*rl + this.M[2][1]*gl + this.M[2][2]*bl;

        const wp = this.whitePoints[this.currentIlluminant];
        const xr = X / wp.Xn, yr = Y / wp.Yn, zr = Z / wp.Zn;

        const f = t => t > 0.008856 ? Math.pow(t, 1/3) : (7.787 * t) + (16 / 116);

        const fx = f(xr), fy = f(yr), fz = f(zr);

        const L = (116 * fy) - 16;
        const a = 500 * (fx - fy);
        const bb = 200 * (fy - fz);

        return [Math.round(L), Math.round(a), Math.round(bb)];
    }


    labToRgb(L, a, b) {
        const wp = this.whitePoints[this.currentIlluminant];
        const fy = (L + 16) / 116;
        const fx = a / 500 + fy;
        const fz = fy - b / 200;

        const fInv = t => Math.pow(t, 3) > 0.008856 ? Math.pow(t, 3) : (116 * t - 16) / 903.3;

        const X = fInv(fx) * wp.Xn / 100;
        const Y = fInv(fy) * wp.Yn / 100;
        const Z = fInv(fz) * wp.Zn / 100;

        const rl = this.invM[0][0]*X + this.invM[0][1]*Y + this.invM[0][2]*Z;
        const gl = this.invM[1][0]*X + this.invM[1][1]*Y + this.invM[1][2]*Z;
        const bl = this.invM[2][0]*X + this.invM[2][1]*Y + this.invM[2][2]*Z;

        return this.handleGamut(rl, gl, bl);
    }
}

function runModelTests() {
    const model = new ColorModel();
    console.log("=== Запуск микро-тестов математики ===");
    
    const hsvTest = model.rgbToHsv(255, 0, 0);
    const hsvPass = hsvTest[0] === 0 && hsvTest[1] === 100 && hsvTest[2] === 100;
    console.log(`Тест 1 (RGB -> HSV для Красного): Получено [${hsvTest}], Ожидалось [0, 100, 100] -> ${hsvPass ? '✅ ПРОЙДЕН' : '❌ ОШИБКА'}`);

    const rgbTest = model.hsvToRgb(0, 100, 100);
    const rgbPass = rgbTest[0] === 255 && rgbTest[1] === 0 && rgbTest[2] === 0;
    console.log(`Тест 2 (HSV -> RGB для Красного): Получено [${rgbTest}], Ожидалось [255, 0, 0] -> ${rgbPass ? '✅ ПРОЙДЕН' : '❌ ОШИБКА'}`);

    model.currentIlluminant = 'D65';
    model.updateMatrices();
    const labTest = model.rgbToLab(255, 0, 0);
    const labPass = Math.abs(labTest[0] - 53) <= 2 && Math.abs(labTest[1] - 80) <= 2 && Math.abs(labTest[2] - 67) <= 2;
    console.log(`Тест 3 (RGB -> LAB D65 для Красного): Получено [${labTest}], Ожидалось близко к [53, 80, 67] -> ${labPass ? '✅ ПРОЙДЕН' : '❌ ОШИБКА'}`);
    
    return hsvPass && rgbPass && labPass;
}

setTimeout(runModelTests, 500);
