#include <mkl_service.h>
#include <mkl.h>

#include <iostream>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;

namespace Native
{
	extern "C"
	{
		DllExport void Transpose(size_t rows, size_t cols, MKL_Complex16* data)
		{
			MKL_Complex16 one = { 1, 0 };
			
			mkl_zimatcopy('R', 'T', rows, cols, one, data, cols, rows);
		}

		DllExport void TransposeBlock(size_t rows, size_t cols, MKL_Complex16* data, size_t lda, size_t ldb)
		{
			MKL_Complex16 one = { 1, 0 };

			mkl_zimatcopy('R', 'T', rows, cols, one, data, lda, ldb);
		}
	}
}