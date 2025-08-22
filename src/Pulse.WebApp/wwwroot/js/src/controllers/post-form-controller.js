import { Controller } from "framework";
import { exponentialInterpolation } from "../utils/maths.js";

export default class PostFormController extends Controller {
  constructor(context) {
    super(context);
  }

  connect() {
    this.postBox.addEventListener("input", this._handleInput.bind(this));
    this._handleInput();
  }

  _handleInput() {
    const text = this.postBox.context.value;
    const textLength = text.length;
    const maxLength = parseInt(this.postMaxLength);

    this.postBox.context.style.height = "auto";
    this.postBox.context.style.height = `${this.postBox.context.scrollHeight}px`;

    this.postLength = textLength;

    if (textLength <= maxLength && textLength > 0) {
      this.lengthIndicator.context.style.display = "block";
      const startColour = this._parseColorArray(this.postStartColour);
      const midColour = this._parseColorArray(this.postMidColour);
      const endColour = this._parseColorArray(this.postEndColour);
      const progress = textLength / maxLength;
      const progressPercentage = progress * 100;
      const strokeColor = this._getCurrentColourValue(
        startColour,
        midColour,
        endColour,
        progress
      );

      this.lengthIndicator.context.style.strokeDasharray = `${progressPercentage} 100`;
      this.lengthIndicator.context.style.stroke = strokeColor;
    } else {
      this.lengthIndicator.context.style.display = "none";
    }
  }

  _parseColorArray(colorString) {
    return JSON.parse(colorString);
  }

  _getCurrentColourValue(startColour, midColour, endColour, progress) {
    if (progress <= 0) return `rgb(${startColour.join(", ")})`;
    if (progress >= 1) return `rgb(${endColour.join(", ")})`;

    let adjustedProgress =
      progress <= 0.5 ? progress * 2 : (progress - 0.5) * 2;
    let fromColour = progress <= 0.5 ? startColour : midColour;
    let toColour = progress <= 0.5 ? midColour : endColour;

    let interpolatedRGB = fromColour.map((channel, index) => {
      return exponentialInterpolation(
        channel,
        toColour[index],
        adjustedProgress
      );
    });

    return `rgb(${interpolatedRGB.join(", ")})`;
  }
}
