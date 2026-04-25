#include "LightStrip.h"



LightStrip::LightStrip(std::string name, int pin, int ledCount)
 : Component(name), ledCount(ledCount), drawingMemory(new uint32_t[ledCount]), displayMemory(new byte[ledCount * 16]), strip(ledCount, displayMemory, drawingMemory, pin, WS2812_GRB) {

    for(int i = 0; i < ledCount; i++){
        pixelBuffer.emplace_back(Color(0,0,0));
    }

    changed = true;

    strip.begin();

}


void LightStrip::setPixel(int index, uint32_t color){
    changed = true;

    pixelBuffer[index] = color;
}

void LightStrip::setAll(uint32_t color){
    changed = true;
    
    for(int i = 0; i < ledCount; i++){
        pixelBuffer[i] = color;
    }
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
    scrambled = false;

    for(int i = 0; i < 5; i++){
        setPixel(i, Color(255, 255, 255));
    }

    strip.setBrightness(100);
}

void LightStrip::update(){
    if(scrambled && millis() - lastScrambleUpdate > 50){
        lastScrambleUpdate = millis();
        for(int i = 0; i < ledCount; i++){
            int j = rand() % 4;
            setPixel(i, colourMap[j]);
        }
    }else if(changed){
        changed = false;

        for(int i = 0; i < ledCount; i++){
            strip.setPixel(i, pixelBuffer[i]);
        }
    }

    strip.show();
}

void LightStrip::scramble(){
    scrambled = true;
}