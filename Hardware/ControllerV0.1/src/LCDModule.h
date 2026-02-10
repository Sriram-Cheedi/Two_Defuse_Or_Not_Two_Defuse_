#ifndef LCDMODULE_H
#define LCDMODULE_H

class LCDModule {
public:
    LCDModule();
    void LCDSetup();
    void LCDScroll();
    void LCDSet(char word[]);
};

#endif