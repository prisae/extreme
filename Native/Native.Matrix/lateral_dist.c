//Copyright (c) 2016 by ETH Zurich, Alexey Geraskin, Mikhail Kruglyakov, and Alexey Kuvshinov
//#include <iostream>

#include "algebra.h"
#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

//using namespace std;

//namespace Native
//{
//	extern "C"
//	{
		DllExport void Transpose(size_t rows, size_t cols, complex16* data)
		{
#ifdef _MKL_H_
			MKL_Complex16 one ={1,0};	
                        mkl_zimatcopy('R', 'T', rows, cols, one,(complex_ptr) data, cols, rows);

#else
			complex16 one = { 1, 0 };
			cblas_zimatcopy(CblasRowMajor, CblasTrans, (blasint)rows, (blasint)cols, (complex_ptr)&one,(complex_ptr) data,(blasint) cols,(blasint) rows);
#endif
		}

		DllExport void TransposeBlock(size_t rows, size_t cols, complex16* data, size_t lda, size_t ldb)
		{

#ifdef _MKL_H_	
			MKL_Complex16 one ={1,0};	
                        mkl_zimatcopy('R', 'T', rows, cols, one, (complex_ptr) data, lda, ldb);


#else
			complex16 one = { 1, 0 };
			cblas_zimatcopy(CblasRowMajor, CblasTrans, (blasint)rows,(blasint) cols,(complex_ptr) &one, (complex_ptr) data, (blasint)lda,(blasint) ldb);
#endif
		}
//	}
//}
