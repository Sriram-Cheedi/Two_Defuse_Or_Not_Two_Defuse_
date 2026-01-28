#pragma once
#include <string>
#include "component.h"

class SerialController {

public:

    SerialController();

    void serialWrite(std::string cmpName, std::string args);
    std::string serialRecieve();

};