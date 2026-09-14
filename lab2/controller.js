class ColorController {
    constructor(model) {
        this.model = model;
        this.currentRgb =[255, 0, 0];
    }


    init(view) {
        this.view = view;
        this.updateAllComponents('rgb', this.currentRgb);
    }


    updateAllComponents(sourceModel, values) {
        let rgb = [];
        let hsv = [];
        let lab = [];
        let isClipped = false;

        if (sourceModel === 'rgb') {
            rgb = [...values];
            hsv = this.model.rgbToHsv(rgb[0], rgb[1], rgb[2]);
            lab = this.model.rgbToLab(rgb[0], rgb[1], rgb[2]);
        } else if (sourceModel === 'hsv') {
            hsv = [...values];
            rgb = this.model.hsvToRgb(hsv[0], hsv[1], hsv[2]);
            lab = this.model.rgbToLab(rgb[0], rgb[1], rgb[2]);
        } else if (sourceModel === 'lab') {
            lab = [...values];
            const result = this.model.labToRgb(lab[0], lab[1], lab[2]);
            rgb = result.rgb;
            isClipped = result.clipped; 
            hsv = this.model.rgbToHsv(rgb[0], rgb[1], rgb[2]);
        }

        this.currentRgb = rgb;

        this.view.updateUIValues({ rgb, hsv, lab }, isClipped);

        this.renderSliderGradients(rgb, hsv, lab);
    }

    changeIlluminant(newIlluminant) {
        this.model.currentIlluminant = newIlluminant;
        this.model.updateMatrices();
        this.updateAllComponents('rgb', this.currentRgb);
    }


    changeGamutStrategy(newStrategy) {
        this.model.gamutStrategy = newStrategy;
        this.updateAllComponents('rgb', this.currentRgb);
    }


    renderSliderGradients(rgb, hsv, lab) {
        this.view.setSliderBackground('r', `linear-gradient(to right, rgb(0, ${rgb[1]}, ${rgb[2]}), rgb(255, ${rgb[1]}, ${rgb[2]}))`);
        this.view.setSliderBackground('g', `linear-gradient(to right, rgb(${rgb[0]}, 0, ${rgb[2]}), rgb(${rgb[0]}, 255, ${rgb[2]}))`);
        this.view.setSliderBackground('b', `linear-gradient(to right, rgb(${rgb[0]}, ${rgb[1]}, 0), rgb(${rgb[0]}, ${rgb[1]}, 255))`);

        let hStops = [];
        for (let i = 0; i <= 360; i += 60) {
            let c = this.model.hsvToRgb(i, hsv[1], hsv[2]);
            hStops.push(`rgb(${c[0]},${c[1]},${c[2]})`);
        }
        this.view.setSliderBackground('h', `linear-gradient(to right, ${hStops.join(', ')})`);
        

        let s0 = this.model.hsvToRgb(hsv[0], 0, hsv[2]);
        let s100 = this.model.hsvToRgb(hsv[0], 100, hsv[2]);
        this.view.setSliderBackground('s', `linear-gradient(to right, rgb(${s0.join(',')}), rgb(${s100.join(',')}))`);

        let v0 = this.model.hsvToRgb(hsv[0], hsv[1], 0);
        let v100 = this.model.hsvToRgb(hsv[0], hsv[1], 100);
        this.view.setSliderBackground('v', `linear-gradient(to right, rgb(${v0.join(',')}), rgb(${v100.join(',')}))`);

        let l0 = this.model.labToRgb(0, lab[1], lab[2]).rgb;
        let l100 = this.model.labToRgb(100, lab[1], lab[2]).rgb;
        this.view.setSliderBackground('l', `linear-gradient(to right, rgb(${l0.join(',')}), rgb(${l100.join(',')}))`);

        let aMin = this.model.labToRgb(lab[0], -128, lab[2]).rgb;
        let aMax = this.model.labToRgb(lab[0], 128, lab[2]).rgb;
        this.view.setSliderBackground('la', `linear-gradient(to right, rgb(${aMin.join(',')}), rgb(${aMax.join(',')}))`);

        let bMin = this.model.labToRgb(lab[0], lab[1], -128).rgb;
        let bMax = this.model.labToRgb(lab[0], lab[1], 128).rgb;
        this.view.setSliderBackground('lb', `linear-gradient(to right, rgb(${bMin.join(',')}), rgb(${bMax.join(',')}))`);
    }
}
