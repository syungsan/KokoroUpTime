# -*- coding: sjis -*-

# ���̋N���X�N���v�g�̃p�X�����ƂȂ�
BASE_ABSOLUTE_PATH = File.dirname(File.expand_path('.', __FILE__))

Dir.chdir("#{BASE_ABSOLUTE_PATH}/netcoreapp3.1") do

  # cd ��ł̏���������
  system("#{BASE_ABSOLUTE_PATH}/netcoreapp3.1/KokoroUpTime.exe")
end

exit

