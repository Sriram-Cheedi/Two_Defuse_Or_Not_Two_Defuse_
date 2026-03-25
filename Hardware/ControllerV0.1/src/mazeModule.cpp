#include <mazeModule.h>

MazeModule::MazeModule(std::string name, SerialController serialController, LCDModule& lcd) 
: Component(name), serialController(serialController), lcd(lcd)
{}

void MazeModule::set(std::vector<std::string> args)
{
    if(args.size() == 1){
        if(std::stoi(args[0]) == 0){
            state = 0;
            lcd.set({"\"\"", "\"\""});
        }else{
            state = 1;
        }
    }else{
        
        pageLength = std::ceil(args.size()/16.0);

        route = std::vector<std::vector<int>>(pageLength);

        for (int i = 0; i < pageLength; i++) {

            int startIndex = i * 16;
            int endIndex   = std::min((int)args.size(), startIndex + 16);
            int pageSize   = endIndex - startIndex;

            route[i] = std::vector<int>(pageSize);

            for (int j = 0; j < pageSize; j++) {
                std::string &val = args[startIndex + j];

                if (val == "U")      route[i][j] = 0;
                else if (val == "R") route[i][j] = 1;
                else if (val == "D") route[i][j] = 2;
                else if (val == "L") route[i][j] = 3;
            }
        }
    }
}

void MazeModule::status()
{
    serialController.serialWrite(name, std::to_string(state));
}

void MazeModule::disable()
{
    enabled = false;
}

void MazeModule::enable()
{
    enabled = true;
}

void MazeModule::update()
{
    if(state == 0 || !enabled || route.size() == 0) return;

    int32_t now = millis();
    if (now - lastUpdate < refreshInterval) return;
    lastUpdate = now;

    std::string routeString = ""; 

    for(size_t i = 0; i < route[currentPage].size(); i++){
        if(route[currentPage][i] == 0){
            routeString.push_back('\x00');
        }else if(route[currentPage][i] == 1){
            routeString.push_back('\x03');
        }else if(route[currentPage][i] == 2){
            routeString.push_back('\x01');
        }else if(route[currentPage][i] == 3){
            routeString.push_back('\x02');
        }
    }

    lcd.set({"\"Maze Route (" + std::to_string(currentPage + 1) + "/" + std::to_string(pageLength) + ")\"", routeString});

    currentPage = (currentPage + 1) % pageLength;
}

void MazeModule::scramble()
{
    enabled = false;
}
