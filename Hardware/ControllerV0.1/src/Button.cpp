#include "Button.h"
#include <Arduino.h>


Button::Button(std::string name, int pin, SerialController serialController, uint32_t debounceTime) :
Component(name), pin(pin), serialController(serialController), debounceDelay(debounceTime){

    pinMode(pin, INPUT_PULLDOWN);

    delay(1);

    lastState = digitalRead(pin);
}

Button::Button(std::string name, int pin, SerialController serialController, std::function<void (std::string, int)> onPressed, uint32_t debounceTime) :
Component(name), pin(pin), serialController(serialController), onPressed(onPressed), debounceDelay(debounceTime) {

    pinMode(pin, INPUT_PULLDOWN);

    delay(1);

    lastState = digitalRead(pin);
}

void Button::status(){
    int currentState = digitalRead(pin);
    serialController.serialWrite(name, std::to_string(currentState));
}

void Button::set(std::vector<std::string> args){return;}

void Button::enable(){

    enabled = true;

}

void Button::reset(){
    buttonState = -1;
    lastState = digitalRead(pin);
}

void Button::disable(){

    enabled = false;
}

void Button::update(){

    if(!enabled) return;

    int reading = digitalRead(pin);

    if(reading != lastState){

        lastDebounceTime = millis();
    }

    if((millis() - lastDebounceTime) > debounceDelay
        && buttonState != reading){
        
        if(buttonState == -1){
            buttonState = reading;
        }else{
            buttonState = reading;
            if(onPressed != NULL){
                onPressed(name, buttonState);
            }else {
                serialController.serialWrite(name, std::to_string(buttonState));
            }
        }



        

    }

    lastState = reading;
}