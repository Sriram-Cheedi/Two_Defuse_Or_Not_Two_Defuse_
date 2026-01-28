#include "SerialController.h"
#include <Arduino.h>

SerialController::SerialController(){}

void SerialController::serialWrite(std::string name, std::string args){

    Serial.printf("$%s:%s#", name, args);

}

std::string SerialController::serialRecieve(){

    while(Serial.available()){

        if(Serial.read() != '$') break;

        std::string cmd = std::string(Serial.readStringUntil('#').c_str());

        if(cmd != ""){
            return cmd.substr(1);
        }
    }

    return "";
}