#ifndef __WVTEST_CS_H // Blank lines in this file mess up line numbering!
#define __WVTEST_CS_H
#define WVASSERT(x) WvTest.test(WvTest.booleanize(x), __FILE__, __LINE__, #x)
#define WVPASS(x) WVASSERT(x)
#define WVFAIL(x) WvTest.test(!WvTest.booleanize(x), __FILE__, __LINE__, "NOT(" + #x + ")")
#define WVPASSEQ(x, y) WvTest.test_eq((x), (y), __FILE__, __LINE__, #x, #y)
#define WVPASSNE(x, y) WvTest.test_ne((x), (y), __FILE__, __LINE__, #x, #y)
#endif // __WVTEST_CS_H
