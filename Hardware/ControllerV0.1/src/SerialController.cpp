#include "SerialController.h"
#include <Arduino.h>

SerialController::SerialController(){}

void SerialController::serialWrite(std::string name, std::string args){

    Serial.printf("$%s:status %s#", name.c_str(), args.c_str());
}

void join(const std::vector<std::string>& v, char c, std::string& s) {

   s.clear();

   for (std::vector<std::string>::const_iterator p = v.begin();
        p != v.end(); ++p) {
      s += *p;
      if (p != v.end() - 1)
        s += c;
   }
}

void SerialController::serialWrite(std::string name, std::vector<std::string> args){

    std::string combinedArgs;

    join(args, ' ', combinedArgs);

    Serial.printf("$%s:status %s#", name.c_str(), combinedArgs.c_str());
}

std::string SerialController::serialRecieve(){

    while(Serial.available()){

        if(Serial.read() != '$') break;

        std::string cmd = std::string(Serial.readStringUntil('#').c_str());

        if(cmd != ""){
            return cmd;
        }
    }

    return "";
}