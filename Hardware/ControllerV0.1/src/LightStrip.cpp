#include "LightStrip.h"



LightStrip::LightStrip(std::string name, int pin, int ledCount)
 : Component(name), strip(ledCount, displayMemory, drawingMemory, pin, WS2812_GRB) {

    strip.begin();

}


void LightStrip::setPixel(int index, uint32_t color){
    changed = true;

    pixelBuffer[index] = color;
}

void LightStrip::set(std::vector<std::string> args){

}

void LightStrip::status(){
    
}

void LightStrip::disable(){
    changed = true;

    strip.setBrightness(0);
}

void LightStrip::enable(){
    changed = true;

    strip.setBrightness(100);
}

void LightStrip::update(){
    if(changed){
        changed = false;

        for(int i = 0; i < ledCount; i++){
            strip.setPixelColor(i, pixelBuffer[i]);
        }
    }

    strip.show();
}

void LightStrip::scramble(){

}