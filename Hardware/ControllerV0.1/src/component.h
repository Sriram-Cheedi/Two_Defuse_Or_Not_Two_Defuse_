#pragma once
#include <string>
#include <vector>

class Component{

public:

    Component(std::string name) : name(name) {};

    std::string name;

    virtual void set(std::vector<std::string> args) = 0;
    virtual void status() = 0;

};