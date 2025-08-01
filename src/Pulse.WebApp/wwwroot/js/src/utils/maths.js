export function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}

export function exponentialInterpolation(startValue, endValue, progress) {
    const valueRange = endValue - startValue;
    const exponentialFactor = Math.pow(2, 10 * (progress - 1));
    return Math.round(startValue + valueRange * exponentialFactor);
}
