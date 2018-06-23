#include "stdafx.h"
#include "CppUnitTest.h"

#include <DirectCommunicationService.h>
#include <Rml.RedirectLoadAssemblyFolder.Clr.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace Rml::Communication::Direct::Clr;
using namespace Rml::Communication::Clr;

namespace RmlCommunicationDirectClrTest
{		
    TEST_CLASS(UnitTest1)
    {
    public:
        
        TEST_METHOD(TestMethod1)
        {
            Rml::RedirectLoadAssemblyFolder::RedirectExecutingAssemblyFolder();

            char* data = new char[2];
            data[0] = 1;
            data[1] = 2;

            auto a = new DirectCommunicationService();
            auto b = new DirectCommunicationService();

            a->SetTarget(b);
            auto recvA = false;
            auto recvB = false;
            a->SetReceiveCallback(ReceiveCallback::Create([&data,&recvA](void* buffer, int size)
            {
                if (memcmp(buffer, data, size) == 0)
                {
                    recvA = true;
                }
            }));
            b->SetReceiveCallback(ReceiveCallback::Create([&data,&recvB](void* buffer, int size)
            {
                if (memcmp(buffer, data, size) == 0)
                {
                    recvB = true;
                }
            }));

            a->Send(data, 2);
            b->Send(data, 2);

            Assert::IsTrue(recvA);
            Assert::IsTrue(recvB);
        }

    };
}