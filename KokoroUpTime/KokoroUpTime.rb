# -*- coding: sjis -*-

# ���̋N���X�N���v�g�̃p�X�����ƂȂ�
BASE_ABSOLUTE_PATH = File.dirname(File.expand_path('.', __FILE__))

Dir.chdir("#{BASE_ABSOLUTE_PATH}/KokoroUpTime") do

  # cd ��ł̏���������
  system("#{BASE_ABSOLUTE_PATH}/KokoroUpTime/KokoroUpTime.exe")
end

exit
