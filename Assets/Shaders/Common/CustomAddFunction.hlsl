#ifndef CUSTOM_ADD_NODE_INCLUDED
#define CUSTOM_ADD_NODE_INCLUDED

void CustomAdd_float(in float A, in float B, out float Result)
{
    Result = A + B;
}

void CustomAdd_half(in half A, in half B, out half Result)
{
    Result= A+ B;
}

#endif