#include "Component.h"
#include "SerialController.h"
#include <functional>

class Button : public Component{

private:

    const int pin;
    SerialController serialController;
    std::function<void (void)> onPressed;

    int lastState;

public:

    void status() override;
    void set(std::vector<std::string> args) override;

    Button(std::string name, int pin, SerialController serialController);
    Button(std::string name, int pin, SerialController serialController, std::function<void (void)> onPressed);

    int getState();
    void update();

};