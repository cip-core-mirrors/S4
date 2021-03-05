//   Copyright 2021 Absa Group
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
namespace ABSA.RD.S4.S3Bench.Settings
{
    public class S3Settings
    {
        public string Region { get; set; }
        public string Endpoint { get; set; }
        public string Bucket { get; set; }
        public string Prefix { get; set; }
        public string KmsKey { get; set; }

        public string CredentialsProfile { get; set; }

        public bool NoProxy { get; set; }
        public int TimeoutSeconds { get; set; }
    }
}