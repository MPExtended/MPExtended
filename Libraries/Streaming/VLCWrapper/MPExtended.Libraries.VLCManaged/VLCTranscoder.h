// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.

#pragma once

#include "common.h"
#include "VLCNative.h"

using namespace System;

namespace MPExtended {
	namespace Libraries {
		namespace VLCManaged {
			public enum PlayerState {
				New = 0,
				Created = 1,
				Started = 2,
				Playing = 3,
				Error = 4,
				Finished = 5
			};

			public ref class VLCTranscoder sealed {
			public:
				VLCTranscoder();

				void SetArguments(array<String^>^ args);
				void SetInput(String ^ input);
				void SetSout(String ^ sout);
				void SetMediaName(String ^ medianame);
				void StartTranscoding();

				void Seek(float position);

				PlayerState GetPlayerState();
				float GetPosition();
				int GetTime();

				void StopTranscoding();
			private:
				VLCNative* vlc;
			};
		}
	}
}
